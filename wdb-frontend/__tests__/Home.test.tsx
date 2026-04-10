import { render, screen } from '@testing-library/react';
import Home from "../app/page"

describe('Home page', () => {
  it('should display the project title', () => {
    render(<Home/>);
    expect(screen.getByText('Worker Data Blockchain')).toBeInTheDocument();
  });
});